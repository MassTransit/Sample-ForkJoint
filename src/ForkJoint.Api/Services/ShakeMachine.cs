namespace ForkJoint.Api.Services;

using System.Threading.Tasks;
using Contracts;


public class ShakeMachine :
    IShakeMachine
{
    public async Task PourShake(string flavor, Size size)
    {
        await Task.Delay(1000);
    }
}