namespace ForkJoint.Api.Services
{
    using System.Threading.Tasks;


    public interface IFryer
    {
        public Task FryOnionRings(int quantity);
    }
}