using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.Offers;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers.Admin;

[Route("api/admin/offers")]
public class OffersController : AdminControllerBase
{
    private readonly IOfferService _offerService;

    public OffersController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OfferDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _offerService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOfferRequest request, CancellationToken cancellationToken)
    {
        var result = await _offerService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOfferRequest request, CancellationToken cancellationToken)
    {
        var result = await _offerService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _offerService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
