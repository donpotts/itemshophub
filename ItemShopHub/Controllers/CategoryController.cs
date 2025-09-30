using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ItemShopHub.Data;
using ItemShopHub.Shared.Models;

namespace ItemShopHub.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class CategoryController(ApplicationDbContext ctx) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<Category>> Get()
    {
        return Ok(ctx.Category.Include(x => x.Product));
    }

    [HttpGet("{key}")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetAsync(long key)
    {
        var category = await ctx.Category.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == key);

        if (category == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(category);
        }
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Category>> PostAsync(Category category)
    {
        var record = await ctx.Category.FindAsync(category.Id);
        if (record != null)
        {
            return Conflict();
        }
    
        // Capture related product IDs and reset navigation collection instead of assigning null
        var productIds = category.Product.Select(y => y.Id).ToList();
        category.Product = new();

        await ctx.Category.AddAsync(category);

        if (productIds.Count > 0)
        {
            var newValues = await ctx.Product.Where(x => productIds.Contains(x.Id)).ToListAsync();
            category.Product = [..newValues];
        }

        await ctx.SaveChangesAsync();

        return Created($"/category/{category.Id}", category);
    }

    [HttpPut("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> PutAsync(long key, Category update)
    {
        var category = await ctx.Category.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == key);

        if (category == null)
        {
            return NotFound();
        }

        ctx.Entry(category).CurrentValues.SetValues(update);

        if (update.Product != null)
        {
            var updateValues = update.Product.Select(x => x.Id);
            category.Product ??= [];
            category.Product.RemoveAll(x => !updateValues.Contains(x.Id));
            var addValues = updateValues.Where(x => !category.Product.Select(y => y.Id).Contains(x));
            var newValues = await ctx.Product.Where(x => addValues.Contains(x.Id)).ToListAsync();
            category.Product.AddRange(newValues);
        }

        await ctx.SaveChangesAsync();

        return Ok(category);
    }

    [HttpPatch("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> PatchAsync(long key, Delta<Category> delta)
    {
        var category = await ctx.Category.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == key);

        if (category == null)
        {
            return NotFound();
        }

        delta.Patch(category);

        await ctx.SaveChangesAsync();

        return Ok(category);
    }

    [HttpDelete("{key}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(long key)
    {
        var category = await ctx.Category.FindAsync(key);

        if (category != null)
        {
            ctx.Category.Remove(category);
            await ctx.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("bulk-upsert")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<object>> BulkUpsertAsync(List<Category> categories)
    {
        if (categories == null || !categories.Any())
        {
            return BadRequest("No categories provided");
        }

        var processedCount = 0;
        var addedCount = 0;
        var updatedCount = 0;
        var errors = new List<string>();

        try
        {
            foreach (var category in categories)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    if (category.Id.HasValue && category.Id.Value > 0)
                    {
                        var existingCategory = await ctx.Category.FindAsync(category.Id.Value);
                        if (existingCategory != null)
                        {
                            existingCategory.Name = category.Name;
                            existingCategory.Description = category.Description;
                            existingCategory.IconUrl = category.IconUrl;
                            existingCategory.ParentCategoryId = category.ParentCategoryId;
                            existingCategory.Notes = category.Notes;
                            existingCategory.UserId = category.UserId;
                            existingCategory.ModifiedDate = now;
                            ctx.Category.Update(existingCategory);
                            updatedCount++;
                        }
                        else
                        {
                            category.CreatedDate = now;
                            category.ModifiedDate = now;
                            await ctx.Category.AddAsync(category);
                            addedCount++;
                        }
                    }
                    else
                    {
                        var existingByName = await ctx.Category
                            .FirstOrDefaultAsync(c => c.Name == category.Name && !string.IsNullOrEmpty(category.Name));
                        if (existingByName != null)
                        {
                            existingByName.Description = category.Description;
                            existingByName.IconUrl = category.IconUrl;
                            existingByName.ParentCategoryId = category.ParentCategoryId;
                            existingByName.Notes = category.Notes;
                            existingByName.UserId = category.UserId;
                            existingByName.ModifiedDate = now;
                            ctx.Category.Update(existingByName);
                            updatedCount++;
                        }
                        else
                        {
                            category.CreatedDate = now;
                            category.ModifiedDate = now;
                            await ctx.Category.AddAsync(category);
                            addedCount++;
                        }
                    }
                    processedCount++;
                }
                catch (Exception ex)
                {
                    var categoryName = category?.Name ?? "Unknown";
                    errors.Add($"Row {processedCount + 1} (Category: {categoryName}): {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        errors.Add($"  Inner error: {ex.InnerException.Message}");
                    }
                }
            }
            await ctx.SaveChangesAsync();
            return Ok(new { ProcessedCount = processedCount, AddedCount = addedCount, UpdatedCount = updatedCount, Errors = errors, Success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ProcessedCount = processedCount, AddedCount = addedCount, UpdatedCount = updatedCount, Errors = new[] { ex.Message }, Success = false });
        }
    }
}
