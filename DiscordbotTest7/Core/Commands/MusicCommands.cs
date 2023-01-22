using Discord.Commands;
using System.Threading.Tasks;
using DiscordbotTest7.Core.Managers;
using Discord;
using Discord.WebSocket;

namespace DiscordbotTest7.Core.Commands
{
    [Name("Music")]
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        [Command("join")]
        public async Task JoinCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("play")]
        public async Task PlayCommand([Remainder] string search)
            => await Context.Channel.SendMessageAsync(await AudioManager.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search, Context.Channel as ITextChannel));

        [Command("leave")]
        public async Task LeaveCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.LeaveAsync(Context.Guild));

        [Command("pause")]
        public async Task PauseCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.PauseAsync(Context.Guild));

        [Command("resume")]
        public async Task ResumeCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.ResumeAsync(Context.Guild));

        [Command("stop")]
        public async Task StopCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.StopAsync(Context.Guild));

        [Command("skip")]
        public async Task SkipCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.SkipAsync(Context.Guild));

        [Command("volume")]
        public async Task VolumeCommand(ushort i)
            => await Context.Channel.SendMessageAsync(await AudioManager.VolumeAsync(i, Context.Guild));

        [Command("seek")]
        public async Task SeekCommand(TimeSpan t)
            => await Context.Channel.SendMessageAsync(await AudioManager.SeekAsync(t, Context.Guild));

        [Command("queue")]
        public async Task QueueCommand()
            => await AudioManager.QueueAsync(Context.Guild, Context.Channel as ITextChannel);

        [Command("loop")]
        public async Task LoopCommand()
        {
            if (AudioManager.loop)
            {
                await Context.Channel.SendMessageAsync("Disabled looping");
                AudioManager.loop = false;
            }
            else
            {
                await Context.Channel.SendMessageAsync("Enabled looping");
                AudioManager.loop = true;
            }
        }
    }

    [Name("Regular")]
    public class RegularCommands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        => await Context.Channel.SendMessageAsync("Pong!"); 

        [Command("connect")]
        [Summary("connects to lavalink")]
        public async Task Connect() 
            => await AudioManager.ConnectAsync();
    }
}
