using System.ComponentModel.DataAnnotations;

namespace DnaVastgoed.Models {

    public class PostmarkWebhook {

        [Required]
        [EmailAddress]
        public string Recipient { get; set; }

        [Required]
        public bool SuppressSending { get; set; }

    }

}
