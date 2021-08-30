namespace ForkJoint.Application.Services
{
    using Contracts;
    using System.Threading.Tasks;

    public class ShakeMachine :
        IShakeMachine
    {
        public async Task PourShake(string flavor, Size size)
        {
            await Task.Delay(1000);
        }
    }
}