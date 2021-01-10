namespace ForkJoint.Api.Services
{
    using System.Threading.Tasks;


    public class Fryer :
        IFryer
    {
        public async Task FryOnionRings(int quantity)
        {
            await Task.Delay(1000);
        }
    }
}