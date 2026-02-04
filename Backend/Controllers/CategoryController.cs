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
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="500">Something went wrong server side.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponseDTO>), 200)]

    public async Task<ActionResult<IEnumerable<CategoryResponseDTO>>> GetCategories()
    {
      if (_dataContext.Categories == null)
      {
        return NotFound();
      }

      var categories = await _dataContext.Categories
          .Select(t => new CategoryResponseDTO
          {
            Id = t.Id,
            CategoryName = t.CategoryName,
            DefaultDuration = t.DefaultDuration,
            Description = t.Description
          })
          .ToListAsync();

      return categories;
    }
    /// <summary>
    /// Retrieves a category by ID.
    /// </summary>
    /// <param name="Id">The category ID.</param>
    /// <response code="200">Returns the category</response>
    /// <response code="404">If the category is not found</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="500">Something went wrong server side.</response>
    //GET api/Categories/{id}
    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(CategoryResponseDTO), 200)]
    [ProducesResponseType(typeof(ApiNotFoundErrorDTO), 404)]
    public async Task<ActionResult<CategoryResponseDTO>> GetCategory(int Id)
    {
      if (_dataContext.Categories == null)
      {
        return NotFound();
      }
      var Category = await _dataContext.Categories
          .Where(r => r.Id == Id)
          .Select(t => new CategoryResponseDTO
          {
            Id = t.Id,
            CategoryName = t.CategoryName,
            DefaultDuration = t.DefaultDuration,
            Description = t.Description
          })
          .FirstOrDefaultAsync();
      if (Category is null)
      {
        return NotFound(new ApiNotFoundErrorDTO
        {
          StatusCode = 404,
          Message = $"Category with id {Id} was not found."
        });
      }
      return Category;
    }

    /// <summary>
    /// Creates a category
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     {
    ///        "Name": "Developer",
    ///     }
    /// </remarks>
    /// <response code="201">Returns the newly created category</response>
    /// <response code="400">If the category is null</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="500">Something went wrong server side.</response>
    //POST api/Categories
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CategoryResponseDTO), 201)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponseDTO>> AddCategory([FromBody] CreateCategoryDTO dto)
    {
      if (_dataContext.Categories == null)
      {
        return NotFound();
      }
      var entity = new Category
      {
        CategoryName = dto.CategoryName,
        DefaultDuration = dto.DefaultDuration,
        Description = dto.Description
      };

      _dataContext.Categories.Add(entity);
      await _dataContext.SaveChangesAsync();

      var response = await _dataContext.Categories
          .Where(r => r.Id == entity.Id)
          .Select(t => new CategoryResponseDTO
          {
            Id = t.Id,
            CategoryName = t.CategoryName,
            DefaultDuration = t.DefaultDuration,
            Description = t.Description
          })
          .FirstAsync();

      return CreatedAtAction(nameof(GetCategory), new { id = response.Id }, response);
    }
    /// <summary>
    /// Updates a category by its ID.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// {
    ///    "name": "Updated Name",
    /// }
    /// </remarks>
    /// <param name="Id">Category ID</param>
    /// <response code="204">Confirms update with status code.</response>
    /// <response code="401">If you lack an jwt token in your request headers</response>
    /// <response code="404">If the category can't be found</response>
    /// <response code="500">Something went wrong server side.</response>
    //PUT api/Categories/{id}
    [HttpPut("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiNotFoundErrorDTO), 404)]
    public async Task<IActionResult> UpdateCategory(int Id, UpdateCategoryDTO dto)
    {
      if (_dataContext.Categories == null)
      {
        return NotFound();
      }
      var entity = await _dataContext.Categories.FindAsync(Id);
      if (entity == null)
      {
        return NotFound(new ApiNotFoundErrorDTO
        {
          StatusCode = 404,
          Message = $"Category with id {Id} was not found."
        });
      }

      if (dto.CategoryName != null)
        entity.CategoryName = dto.CategoryName;

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
    //DELETE api/Categories/{id}
    [HttpDelete("{Id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiNotFoundErrorDTO), 404)]
    public async Task<ActionResult> DeleteCategory(int Id)
    {
      if (_dataContext.Categories == null)
      {
        return NotFound();
      }
      var Category = await _dataContext.Categories.FindAsync(Id);
      if (Category is null)
      {
        return NotFound(new ApiNotFoundErrorDTO
        {
          StatusCode = 404,
          Message = $"Category with id {Id} was not found."
        });
      }
      _dataContext.Categories.Remove(Category);
      await _dataContext.SaveChangesAsync();
      return NoContent();
    }
  }
}