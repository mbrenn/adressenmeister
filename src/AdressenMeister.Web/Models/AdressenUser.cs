namespace AdressenMeister.Web.Models
{
    public class AdressenUser
    {
        public string? name { get; set; }
        
        public string? prename { get; set; }
        
        public string? street { get; set; }
        
        public string? zipcode { get; set; }
        
        public string? city { get; set; }
        
        public string? country { get; set; }
        
        public string? phone { get; set; }
        
        public string? email { get; set; }
        
        public bool isNameVisible { get; set; }
        
        public bool isEmailVisible { get; set; }
        
        public bool isAddressVisible { get; set; }

        public bool isPhoneVisible { get; set; }
        
        /// <summary>
        /// Gets or sets the secretkey to access the user
        /// </summary>
        public string? secret { get; set; }
    }
}