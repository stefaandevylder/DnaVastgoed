using System.ComponentModel.DataAnnotations;

namespace DnaVastgoed.Models {

    public class DnaPropertyImage {

        [Key]
        public int Id { get; set; }
        public string Url { get; set; }

        public DnaPropertyImage(string url) {
            Url = url;
        }

    }
}
