using Attributes;
using Newtonsoft.Json;
using Repositories;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class User
    {
        public const string User_Avatars_Folder = @"/Images_Data/User_Avatars/";
        public const string Default_Avatar = @"no_avatar.png";

        [JsonIgnore]
        public static string DefaultImage => User_Avatars_Folder + Default_Avatar;

        public User Clone() => JsonConvert.DeserializeObject<User>(JsonConvert.SerializeObject(this));

        #region Data Members

        public int Id { get; set; } = 0;
        public int UserTypeId { get; set; } = 3;
        public bool Verified { get; set; } = false;
        public bool Blocked { get; set; } = false;

        [Display(Name = "Prenom"), Required(ErrorMessage = "Obligatoire")]
        public string FirstName { get; set; }

        [Display(Name = "Nom"), Required(ErrorMessage = "Obligatoire")]
        public string LastName { get; set; }

        [Display(Name = "Désignation"), Required(ErrorMessage = "Obligatoire")]
        public int GenderId { get; set; }

        [Display(Name = "Courriel"), EmailAddress(ErrorMessage = "Invalide"), Required(ErrorMessage = "Obligatoire")]
        [System.Web.Mvc.Remote("EmailAvailable", "Accounts", HttpMethod = "POST", ErrorMessage = "Ce courriel n'est pas disponible.")]
        public string Email { get; set; }

        [Display(Name = "Avatar")]
        [ImageAsset(User_Avatars_Folder, Default_Avatar)]
        public string Avatar { get; set; } = DefaultImage;

        [Display(Name = "Mot de passe"), Required(ErrorMessage = "Obligatoire")]
        [StringLength(50, ErrorMessage = "Le mot de passe doit comporter au moins {2} caractères.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [JsonIgnore]
        [Display(Name = "Confirmation")]
        [Compare("Email", ErrorMessage = "Le courriel et celui de confirmation ne correspondent pas.")]
        public string ConfirmEmail { get; set; }

        [JsonIgnore]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmation")]
        [Compare("Password", ErrorMessage = "Le mot de passe et celui de confirmation ne correspondent pas.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Date de création")]
        [DataType(DataType.Date)]
        public DateTime CreationDate { get; set; } = DateTime.Now;

        #endregion Data Members

        #region View members

        [JsonIgnore]
        public Gender Gender => DB.GetRepo<Gender>().Get(this.GenderId);

        [JsonIgnore]
        public UserType UserType => DB.GetRepo<UserType>().Get(this.UserTypeId);

        [JsonIgnore]
        public bool IsPowerUser => this.UserTypeId <= 2 /* Admin = 1 , PowerUser = 2 */;

        [JsonIgnore]
        public bool IsAdmin => this.UserTypeId == 1 /* Admin */;

        public string GetFullName(bool showGender = false)
        {
            if (showGender)
            {
                if (this.Gender.Name != "Neutre")
                    return this.Gender.Name + " " + this.LastName;
            }

            return this.FirstName + " " + this.LastName;
        }

        #endregion View members
    }
}