
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Pattern.Infrastructure
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
}