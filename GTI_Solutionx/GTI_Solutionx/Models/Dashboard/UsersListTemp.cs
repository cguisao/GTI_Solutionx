using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GTI_Solutionx.Models.Dashboard
{
    public class UsersListTemp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemID { get; set; }
        public string sku { get; set; }
        public string userID { get; set; }
    }
}
