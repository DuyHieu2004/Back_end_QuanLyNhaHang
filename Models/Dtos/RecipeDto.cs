using System;
using System.Collections.Generic;

namespace QuanLyNhaHang.Models.Dtos
{
    // DTO for complete recipe information
    public class RecipeDto
    {
        public string MaCongThuc { get; set; } = null!;
        public string MaCt { get; set; } = null!;
        public string TenCt { get; set; } = null!;
        public string MaMonAn { get; set; } = null!;
        public string TenMonAn { get; set; } = null!;
        public string MaPhienBan { get; set; } = null!;
        public string TenPhienBan { get; set; } = null!;
        public decimal Gia { get; set; }
        public List<RecipeIngredientDto> Ingredients { get; set; } = new List<RecipeIngredientDto>();
    }

    // DTO for ingredient in a recipe
    public class RecipeIngredientDto
    {
        public long MaChiTietCongThuc { get; set; }
        public string MaNguyenLieu { get; set; } = null!;
        public string TenNguyenLieu { get; set; } = null!;
        public string? DonViTinh { get; set; }
        public int SoLuongCanDung { get; set; }
        public decimal GiaBan { get; set; }
        public decimal ThanhTien { get; set; } // SoLuongCanDung * GiaBan
    }

    // DTO for creating new recipe
    public class CreateRecipeDto
    {
        public string MaCt { get; set; } = null!;
        public string MaPhienBan { get; set; } = null!;
        public decimal Gia { get; set; }
        public List<AddIngredientToRecipeDto>? Ingredients { get; set; }
    }

    // DTO for updating recipe
    public class UpdateRecipeDto
    {
        public string? MaCt { get; set; }
        public string? MaPhienBan { get; set; }
        public decimal? Gia { get; set; }
    }

    // DTO for adding ingredient to recipe
    public class AddIngredientToRecipeDto
    {
        public string MaNguyenLieu { get; set; } = null!;
        public int SoLuongCanDung { get; set; }
    }

    // DTO for ingredient calculation result
    public class CalculateIngredientsDto
    {
        public string MaCongThuc { get; set; } = null!;
        public string TenMonAn { get; set; } = null!;
        public string TenPhienBan { get; set; } = null!;
        public int SoSuatAn { get; set; }
        public List<CalculatedIngredientDto> Ingredients { get; set; } = new List<CalculatedIngredientDto>();
        public decimal TongChiPhi { get; set; }
    }

    // DTO for calculated ingredient
    public class CalculatedIngredientDto
    {
        public string TenNguyenLieu { get; set; } = null!;
        public string? DonViTinh { get; set; }
        public int SoLuongGoc { get; set; }
        public int SoLuongCanDung { get; set; }
        public decimal GiaBan { get; set; }
        public decimal ThanhTien { get; set; }
    }

    // DTO for recipe list item
    public class RecipeListItemDto
    {
        public string MaCongThuc { get; set; } = null!;
        public string TenMonAn { get; set; } = null!;
        public string TenCt { get; set; } = null!;
        public string TenPhienBan { get; set; } = null!;
        public decimal Gia { get; set; }
        public int SoLuongNguyenLieu { get; set; }
    }
}
