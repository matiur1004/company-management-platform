﻿namespace ClientPortal.Data.Entities
{
    using System.Text.Json.Serialization;

    public class User
    {
        public int Id { get; set; }
        public int UmfaId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; } = false;
        [JsonIgnore]
        public string PasswordHash { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }
        public List<AMRScadaUser> AmrScadaUsers { get; set; }
        public List<AMRMeter> AmrMeters { get; set; }
        public List<Building> Buildings { get; set; }
    }
}