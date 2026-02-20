using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementSystem.Server.Repositories;
using VendorManagementSystem.Server.Services;


using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Products;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public ProductsController(IProductRepository productRepository, ICloudinaryService cloudinaryService)
    {
        _productRepository = productRepository;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("{id}/upload-image")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<string>> UploadProductImage(int id, IFormFile file)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id); // Assuming GetProductByIdAsync returns a DTO or entity that can be updated
            if (product == null) return NotFound("Product not found");

            if (file == null || file.Length == 0) return BadRequest("File is empty");
            if (file.Length > 5 * 1024 * 1024) return BadRequest("File size exceeds 5MB limit");
            if (!file.ContentType.StartsWith("image/")) return BadRequest("File is not an image");

            // Assuming ICloudinaryService has an UploadImageAsync method that takes IFormFile
            var imageUrl = await _cloudinaryService.UploadImageAsync(file, "products");
            
            // To update the product's image URL, we need to use the existing UpdateProductAsync method
            // which takes an UpdateProductRequest. We'll construct one from the existing product data.
            var updateRequest = new UpdateProductRequest
            {
                Id = product.Id,
                SKU = product.SKU,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Unit = product.Unit,
                Description = product.Description,
                MinStock = product.MinStock,
                MaxStock = product.MaxStock,
                CurrentStock = product.CurrentStock,
                ImageUrl = imageUrl // Update the image URL
            };

            await _productRepository.UpdateProductAsync(id, updateRequest);

            return Ok(imageUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductListResponse>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null)
    {
        var result = await _productRepository.GetProductsAsync(page, pageSize, search, categoryId);
        return Ok(ApiResponse<ProductListResponse>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
    {
        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(ApiResponse<ProductDto>.Fail("Product not found"));

        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var product = await _productRepository.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, ApiResponse<ProductDto>.Ok(product));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productRepository.UpdateProductAsync(id, request);
        if (product == null)
            return NotFound(ApiResponse<ProductDto>.Fail("Product not found"));

        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(int id)
    {
        var result = await _productRepository.DeleteProductAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Product not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Product deleted"));
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<List<ProductDto>>.Fail("No file uploaded"));

        try
        {
            using var stream = file.OpenReadStream();
            var products = await _productRepository.ImportFromExcelAsync(stream);
            return Ok(ApiResponse<List<ProductDto>>.Ok(products, $"{products.Count} products imported"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ProductDto>>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:int}/upload-image")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file uploaded"));

        var product = await _productRepository.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(ApiResponse<string>.Fail("Product not found"));

        try
        {
            var (url, _) = await _cloudinaryService.UploadFileAsync(file.OpenReadStream(), file.FileName, "product-images");

            var updateReq = new UpdateProductRequest
            {
                Id = id,
                SKU = product.SKU,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Unit = product.Unit,
                Description = product.Description,
                MinStock = product.MinStock,
                MaxStock = product.MaxStock,
                CurrentStock = product.CurrentStock
            };

            await _productRepository.UpdateProductAsync(id, updateReq);

            var dbProduct = await _productRepository.GetProductByIdAsync(id);

            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var entity = await context.Products.FindAsync(id);
            if (entity != null)
            {
                entity.ImageUrl = url;
                await context.SaveChangesAsync();
            }

            return Ok(ApiResponse<string>.Ok(url, "Image uploaded successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }
}
