using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using FluentValidation;

namespace Backend.Controllers
{
  [Route("categories")]
  [Produces("application/json")]
  [Tags("Category")]
  [ApiController]
  public class CategoryController : BaseApiController
  {
    private readonly DataContext _dataContext;

    public CategoryController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves a paged list of all appointment categories.
    /// </summary>
    /// <param name="page">The page number (defaults to 1).</param>
    /// <param name="pageSize">Items per page (max 100, defaults to 100).</param>
    /// <response code="200">Returns a paged wrapper of categories.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponseDTO<CategoryResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
      page = Math.Max(page, 1);
      pageSize = Math.Clamp(pageSize, 1, 100);

      var query = _dataContext.Categories.AsNoTracking();
      var total = await query.CountAsync();

      var data = await query
          .OrderBy(c => c.CategoryName)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .Select(t => new CategoryResponseDTO
          {
            Id = t.Id,
            CategoryName = t.CategoryName,
            DefaultDuration = t.DefaultDuration,
            Description = t.Description
          })
          .ToListAsync();

      return Ok(new PagedResponseDTO<CategoryResponseDTO>
      {
        Data = data,
        Pagination = new PaginationDTO
        {
          Page = page,
          PageSize = pageSize,
          Total = total,
          TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        }
      });
    }

    /// <summary>
    /// Retrieves a specific appointment category by ID.
    /// </summary>
    /// <param name="id">The unique integer ID of the category.</param>
    /// <response code="200">Returns the requested category details.</response>
    /// <response code="404">Not Found: Category ID does not exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponseDTO>> GetCategory(int id)
    {
      var category = await _dataContext.Categories
          .AsNoTracking()
          .Where(r => r.Id == id)
          .Select(t => new CategoryResponseDTO
          {
            Id = t.Id,
            CategoryName = t.CategoryName,
            DefaultDuration = t.DefaultDuration,
            Description = t.Description
          })
          .FirstOrDefaultAsync();

      if (category is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Category {id} not found." });

      return Ok(category);
    }

    /// <summary>
    /// Creates a new appointment category (Admin Only).
    /// </summary>
    /// <param name="dto">The request body.</param>
    /// <param name="validator">The Fluent validation validator.</param>
    /// <response code="201">Success: Returns the newly created category.</response>
    /// <response code="400">Bad Request: Validation failed (e.g., duration out of range).</response>
    /// <response code="401">Unauthorized: Admin JWT required.</response>
    /// <response code="403">Forbidden: Insufficient permissions.</response>
    /// <response code="409">Conflict: Category name already exists.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCategory(
        [FromBody] CreateCategoryDTO dto,
        [FromServices] IValidator<CreateCategoryDTO> validator)
    {
      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid) return ValidationBadRequest(result);

      var normalized = dto.CategoryName.Trim().ToLower();
      if (await _dataContext.Categories.AnyAsync(c => c.CategoryName.ToLower() == normalized))
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = $"Category '{dto.CategoryName}' already exists." });

      var entity = new Category
      {
        CategoryName = dto.CategoryName.Trim(),
        DefaultDuration = dto.DefaultDuration,
        Description = dto.Description
      };

      _dataContext.Categories.Add(entity);
      await _dataContext.SaveChangesAsync();

      return CreatedAtAction(nameof(GetCategory), new { id = entity.Id }, new CategoryResponseDTO
      {
        Id = entity.Id,
        CategoryName = entity.CategoryName,
        DefaultDuration = entity.DefaultDuration,
        Description = entity.Description
      });
    }

    /// <summary>
    /// Partially updates an appointment category (Admin Only).
    /// </summary>
    /// <remarks>
    /// **Partial Update:** Only provide the fields you wish to change.
    /// </remarks>
    /// <param name="id">The unique integer ID of the category.</param>
    /// <param name="dto">The request body.</param>
    /// <param name="validator">The Fluent validation validator.</param>
    /// <response code="200">Success: Returns the updated category object.</response>
    /// <response code="400">Bad Request: Validation failed.</response>
    /// <response code="401">Unauthorized: Admin JWT required.</response>
    /// <response code="404">Not Found: Category ID does not exist.</response>
    /// <response code="409">Conflict: Name collision with another category.</response>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CategoryResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBadRequestErrorDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryDTO dto,
        [FromServices] IValidator<UpdateCategoryDTO> validator)
    {
      var result = await validator.ValidateAsync(dto);
      if (!result.IsValid) return ValidationBadRequest(result);

      var entity = await _dataContext.Categories.FindAsync(id);
      if (entity == null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Category {id} not found." });

      if (!string.IsNullOrWhiteSpace(dto.CategoryName))
      {
        var normalized = dto.CategoryName.Trim().ToLower();
        var exists = await _dataContext.Categories.AnyAsync(c => c.Id != id && c.CategoryName.ToLower() == normalized);
        if (exists) return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Another category with this name exists." });

        entity.CategoryName = dto.CategoryName.Trim();
      }

      if (dto.DefaultDuration.HasValue)
        entity.DefaultDuration = dto.DefaultDuration.Value;

      if (dto.Description != null)
        entity.Description = dto.Description.Trim();

      await _dataContext.SaveChangesAsync();

      return Ok(new CategoryResponseDTO
      {
        Id = entity.Id,
        CategoryName = entity.CategoryName,
        DefaultDuration = entity.DefaultDuration,
        Description = entity.Description
      });
    }

    /// <summary>
    /// Deletes an appointment category (Admin Only).
    /// </summary>
    /// <remarks>
    /// **Safety Guard:** Blocks deletion if any existing appointments are assigned to this category.
    /// </remarks>
    /// <param name="id">The unique integer ID of the category.</param>
    /// <response code="204">Success: Category deleted.</response>
    /// <response code="409">Conflict: Category is currently in use by appointment records.</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorDTO), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
      var category = await _dataContext.Categories.FindAsync(id);
      if (category is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Category {id} not found." });

      var hasAppointments = await _dataContext.Appointments.AnyAsync(a => a.CategoryId == id);
      if (hasAppointments)
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete category: it is referenced by existing appointments." });

      _dataContext.Categories.Remove(category);
      await _dataContext.SaveChangesAsync();

      return NoContent();
    }
  }
}