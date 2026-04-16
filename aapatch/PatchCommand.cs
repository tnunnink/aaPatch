using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace aaPatch;

[Command]
public partial class PatchCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        throw new NotImplementedException();
    }
}