using System;
using System.ComponentModel.DataAnnotations;

namespace DnaVastgoed.Models {

    public class DnaPropertyImage : IEquatable<DnaPropertyImage> {

        [Key]
        public int Id { get; set; }
        public DnaProperty DnaProperty { get; set; }
        public string Url { get; set; }

        public DnaPropertyImage() { }

        public DnaPropertyImage(string url) {
            Url = url;
        }

        public bool Equals(DnaPropertyImage other) {
            if (Url is null) {
                return false;
            }

            return Url.Equals(other.Url);
        }
    }
}
