namespace QLDatVeMayBay.Models
{
    public class DatGhe
    {
            public int IDChuyenBay { get; set; }
            public int TongSoGhe { get; set; }
            public List<int> GheDaDat { get; set; } = new();

        public Dictionary<int, ThongTinGheItem> ThongTinGhe { get; set; } = new();
    }

    public class ThongTinGheItem
    {
        public string HangGhe { get; set; } = "";
        public decimal Gia { get; set; }
    }

}

