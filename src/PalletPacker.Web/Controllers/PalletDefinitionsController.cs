using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PalletPacker.Core.Entities;
using PalletPacker.Infrastructure.Data;
using PalletPacker.Web.Models.PalletDefinitions;

namespace PalletPacker.Web.Controllers;

public class PalletDefinitionsController : Controller
{
    private readonly AppDbContext _dbContext;

    public PalletDefinitionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(bool showArchived = false)
    {
        var definitions = await _dbContext.PalletDefinitions
            .Where(p => showArchived || !p.IsArchived)
            .OrderBy(p => p.Name)
            .Select(p => new PalletDefinitionListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                LengthMm = p.LengthMm,
                WidthMm = p.WidthMm,
                HeightMm = p.HeightMm,
                OwnWeightKg = p.OwnWeightKg,
                MaxWeightKg = p.MaxWeightKg,
                MaxHeightMm = p.MaxHeightMm,
                IsActive = p.IsActive,
                IsArchived = p.IsArchived
            })
            .ToListAsync();

        var model = new PalletDefinitionIndexViewModel
        {
            ShowArchived = showArchived,
            Definitions = definitions
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new PalletDefinitionFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PalletDefinitionFormViewModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var definition = new PalletDefinition
        {
            Name = input.Name,
            PalletType = input.PalletType,
            LengthMm = input.LengthMm,
            WidthMm = input.WidthMm,
            HeightMm = input.HeightMm,
            OwnWeightKg = input.OwnWeightKg,
            MaxWeightKg = input.MaxWeightKg,
            MaxHeightMm = input.MaxHeightMm,
            AllowedOverhangMm = input.AllowedOverhangMm,
            IsActive = input.IsActive,
            IsArchived = false
        };

        _dbContext.PalletDefinitions.Add(definition);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var definition = await _dbContext.PalletDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived);

        if (definition is null)
        {
            return NotFound();
        }

        var input = new PalletDefinitionFormViewModel
        {
            Id = definition.Id,
            Name = definition.Name,
            PalletType = definition.PalletType,
            LengthMm = definition.LengthMm,
            WidthMm = definition.WidthMm,
            HeightMm = definition.HeightMm,
            OwnWeightKg = definition.OwnWeightKg,
            MaxWeightKg = definition.MaxWeightKg,
            MaxHeightMm = definition.MaxHeightMm,
            AllowedOverhangMm = definition.AllowedOverhangMm,
            IsActive = definition.IsActive
        };

        return View(input);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PalletDefinitionFormViewModel input)
    {
        if (id != input.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var definition = await _dbContext.PalletDefinitions
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchived);

        if (definition is null)
        {
            return NotFound();
        }

        definition.Name = input.Name;
        definition.PalletType = input.PalletType;
        definition.LengthMm = input.LengthMm;
        definition.WidthMm = input.WidthMm;
        definition.HeightMm = input.HeightMm;
        definition.OwnWeightKg = input.OwnWeightKg;
        definition.MaxWeightKg = input.MaxWeightKg;
        definition.MaxHeightMm = input.MaxHeightMm;
        definition.AllowedOverhangMm = input.AllowedOverhangMm;
        definition.IsActive = input.IsActive;

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id, bool showArchived)
    {
        var definition = await _dbContext.PalletDefinitions
            .FirstOrDefaultAsync(p => p.Id == id);

        if (definition is null)
        {
            return NotFound();
        }

        definition.IsArchived = true;
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { showArchived });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unarchive(int id, bool showArchived)
    {
        var definition = await _dbContext.PalletDefinitions
            .FirstOrDefaultAsync(p => p.Id == id);

        if (definition is null)
        {
            return NotFound();
        }

        definition.IsArchived = false;
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { showArchived });
    }
}