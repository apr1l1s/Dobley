using System.ComponentModel.DataAnnotations;

namespace Dobley.Domain.Core.Entities.Products;

public enum UnitType
{
    // === ВЕС ===
    [Display(Name = "грамм")]
    Grams,

    [Display(Name = "килограмм")]
    Kilograms,

    [Display(Name = "миллиграмм")]
    Milligrams,

    // === ОБЪЁМ ===
    [Display(Name = "миллилитр")]
    Milliliters,

    [Display(Name = "литр")]
    Liters,

    // === КОЛИЧЕСТВО ===
    [Display(Name = "штука")]
    Pieces,

    [Display(Name = "порция")]
    Servings,

    [Display(Name = "упаковка")]
    Packs,

    [Display(Name = "банка")]
    Jars,

    [Display(Name = "бутылка")]
    Bottles,

    // === ДЛИНА ===
    [Display(Name = "сантиметр")]
    Centimeters
}