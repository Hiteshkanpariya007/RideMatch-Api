using System.ComponentModel.DataAnnotations;

namespace Common.Enums
{
    public enum UserType
    {
        [Display(Name = "Rider")]
        Rider = 1,

        [Display(Name = "Driver")]
        Driver = 2
    }
}
