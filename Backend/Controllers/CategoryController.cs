using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;

namespace Backend.Controllers
{
  [Route("categories")]
  [Produces("application/json")]
  [Tags("Category")]
  [ApiController]
  public class CategoryController : ControllerBase
  {
    private readonly DataContext _dataContext;

    public CategoryController(DataContext dataContext)
    {
      _dataContext = dataContext;
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <returns>A list of categories</returns>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDTO>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryResponseDTO>>> GetCategories()
    {
      //TODO implement pagination
      var categories = await _dataContext.Categories
          .Select(t => new CategoryResponseDTO
          {
            Id = t.Id,
            CategoryName = t.CategoryName,
            DefaultDuration = t.DefaultDuration,
            Description = t.Description
          })
          .ToListAsync();

      return Ok(categories);
    }

    /// <summary>
    /// Retrieves a category by ID.
    /// </summary>
    /// <param name="Id">The category ID.</param>
    /// <response code="200">Returns the category</response>
    /// <response code="404">If the category is not found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(CategoryResponseDTO), 200)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<ActionResult<CategoryResponseDTO>> GetCategory(int Id)
    {
      var category = await _dataContext.Categories
          .Where(r => r.Id == Id)
          .Select(t => new CategoryResponseDTO
          {
            Id = t.Id,
            CategoryName = t.CategoryName,
            DefaultDuration = t.DefaultDuration,
            Description = t.Description
          })
          .FirstOrDefaultAsync();
      if (category is null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Category with id {Id} was not found."
        });
      }
      return Ok(category);
    }

    /// <summary>
    /// Creates a category
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///   "categoryName": "Exsanguination",
    ///   "defaultDuration": 15,
    ///   "description": "Removal of all patients blood to offer to the vampiric overlords."
    /// }
    /// </remarks>
    /// <response code="201">Returns the newly created category</response>
    /// <response code="400">If the category is null</response>
    /// <response code="400">If the category name is null</response>
    /// <response code="409">If the category name already exists</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CategoryResponseDTO), 201)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponseDTO>> AddCategory([FromBody] CreateCategoryDTO dto)
    {
      var categoryName = dto.CategoryName.Trim();

      var nameExists = await _dataContext.Categories.AnyAsync(c =>
          c.CategoryName.ToLower() == categoryName.ToLower());

      if (nameExists)
      {
        return Conflict(new ApiErrorDTO
        {
          StatusCode = 409,
          Message = $"Category with name '{dto.CategoryName}' already exists."
        });
      }

      var entity = new Category
      {
        CategoryName = categoryName,
        DefaultDuration = dto.DefaultDuration,
        Description = dto.Description
      };

      _dataContext.Categories.Add(entity);
      await _dataContext.SaveChangesAsync();

      var response = new CategoryResponseDTO
      {
        Id = entity.Id,
        CategoryName = entity.CategoryName,
        DefaultDuration = entity.DefaultDuration,
        Description = entity.Description
      };

      return CreatedAtAction(nameof(GetCategory), new { Id = response.Id }, response);
    }

    /// <summary>
    /// Updates a category by its ID.
    /// </summary>
    /// <param name="Id">Category ID</param>
    /// <response code="204">Confirms update with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the category can't be found</response>
    /// <response code="409">If the category name already exists</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpPut("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    [ProducesResponseType(typeof(ApiErrorDTO), 409)]
    public async Task<IActionResult> UpdateCategory(int Id, UpdateCategoryDTO dto)
    {
      var entity = await _dataContext.Categories.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiErrorDTO
        {
          StatusCode = 404,
          Message = $"Category with id {Id} was not found."
        });
      }

      if (!string.IsNullOrWhiteSpace(dto.CategoryName) &&
        !string.Equals(dto.CategoryName, entity.CategoryName, StringComparison.OrdinalIgnoreCase))
      {
        var categoryName = dto.CategoryName.Trim();

        var nameExists = await _dataContext.Categories.AnyAsync(c =>
            c.Id != Id &&
            c.CategoryName.ToLower() == categoryName.ToLower());

        if (nameExists)
        {
          return Conflict(new ApiErrorDTO
          {
            StatusCode = 409,
            Message = $"Category with name '{dto.CategoryName}' already exists."
          });
        }

        entity.CategoryName = categoryName;
      }

      if (dto.DefaultDuration.HasValue)
        entity.DefaultDuration = dto.DefaultDuration.Value;

      if (dto.Description is not null)
        entity.Description = dto.Description;

      await _dataContext.SaveChangesAsync();

      return NoContent();
    }

    /// <summary>
    /// Deletes a category
    /// </summary>
    /// <param name="Id">Category ID</param>
    /// <response code="204">Confirms deletion with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the category can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpDelete("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorDTO), 404)]
    public async Task<IActionResult> DeleteCategory(int Id)
    {
      var category = await _dataContext.Categories.FindAsync(Id);
      if (category is null)
        return NotFound(new ApiErrorDTO { StatusCode = 404, Message = $"Category with id {Id} was not found." });

      _dataContext.Categories.Remove(category);

      try
      {
        await _dataContext.SaveChangesAsync();
        return NoContent();
      }
      catch (DbUpdateException)
      {
        return Conflict(new ApiErrorDTO { StatusCode = 409, Message = "Cannot delete category because it is referenced by other records." });
      }
    }
  }
}