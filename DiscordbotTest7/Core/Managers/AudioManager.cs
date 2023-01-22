using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Victoria.Node;
using Victoria.Node.EventArgs;
using Victoria.Player;
using Victoria.Responses;
using Victoria.Responses.Search;

namespace DiscordbotTest7.Core.Managers
{
    public static class AudioManager
    {
        public static bool loop { get; set; }
        private static bool fixVanDeEeuw;
        private static readonly LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();

        public static async Task<string> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel channel)
        {
            if (_lavaNode.HasPlayer(guild)) return "Is already connected to a voicechannel";

            if (voiceState.VoiceChannel is null) return "You must be connected to a voicechannel!";

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, channel);
                return $"Has entered {voiceState.VoiceChannel.Name}";
            }
            catch (Exception ex)
            {
                return $"ERROR\n{ex.Message}";
            }

        }
        public static async Task<string> PlayAsync(SocketGuildUser user, IGuild guild, string searchQuery, ITextChannel channel)
        {
            if (fixVanDeEeuw) fixVanDeEeuw = false;
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return "Please provide search terms.";
            }

            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                var voiceState = user as IVoiceState;
                if (voiceState?.VoiceChannel == null)
                {
                    return "You must be connected to a voice channel!";
                }

                try
                {
                    player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, channel);
                }
                catch (Exception exception)
                {
                    return exception.Message;
                }
            }

            SearchResponse searchResponse = new SearchResponse();

            if (searchQuery[0] == 'h' && searchQuery[1] == 't' && searchQuery[2] == 't')
            {
                searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(searchQuery, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, searchQuery);
            }
            else
            { 
                searchResponse = await _lavaNode.SearchAsync(SearchType.YouTube, searchQuery);
                if (searchResponse.Status == SearchStatus.NoMatches)
                {
                    await channel.SendMessageAsync("Couldn't find anything on Youtube now trying Soundcloud");
                    searchResponse = await _lavaNode.SearchAsync(SearchType.SoundCloud, searchQuery);
                }
            }

            if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
            {
                return $"I wasn't able to find anything for `{searchQuery}`.";
            }

            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                player.Vueue.Enqueue(searchResponse.Tracks);
                await channel.SendMessageAsync($"Envueued {searchResponse.Tracks.Count} songs.");
            }
            else
            {
                var track = searchResponse.Tracks.FirstOrDefault();
                player.Vueue.Enqueue(track);

                await channel.SendMessageAsync($"Envueued {track?.Title}");
            }

            if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
            {
                return "";
            }

            player.Vueue.TryDequeue(out var lavaTrack);
            await player.PlayAsync(lavaTrack);
            return $"now playing {player.Vueue.First<LavaTrack>().Title}";
        }
        public static async Task<string> LeaveAsync(IGuild guild)
        {
            try
            {
                _lavaNode.TryGetPlayer(guild, out var player);
                if (player.PlayerState is PlayerState.Playing) await player.StopAsync();
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\tBot has left a voicechannel");
                return "Left the voicechannel!";
            }
            catch (InvalidOperationException ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        public static async Task ConnectAsync()
        {
            try
            {
                await _lavaNode.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
        public static async Task<string> PauseAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return "I'm not connected to a voice channel.";
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return "I cannot pause when I'm not playing anything!";
            }

            try
            {
                await player.PauseAsync();
                return $"Paused: {player.Track.Title}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        public static async Task<string> ResumeAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return "I'm not connected to a voice channel.";
            }

            if (player.PlayerState != PlayerState.Paused)
            {
                return "I cannot resume when I'm not playing anything!";
            }

            try
            {
                await player.ResumeAsync();
                return $"Resumed: {player.Track.Title}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        public static async Task<string> StopAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return "I'm not connected to a voice channel.";
            }

            if (player.PlayerState == PlayerState.Stopped)
            {
                return "Woaaah there, I can't stop the stopped forced.";
            }

            try
            {
                await player.StopAsync();
                return "No longer playing anything.";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        public static async Task<string> SkipAsync(IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return "I'm not connected to a voice channel.";
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return "Woaaah there, I can't skip when nothing is playing.";
            }

            try
            {
                var (skipped, currenTrack) = await player.SkipAsync(); fixVanDeEeuw = true;
                return $"Skipped: {skipped.Title}\nNow Playing: {currenTrack.Title}";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        public static async Task QueueAsync(IGuild guild, ITextChannel channel)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                channel.SendMessageAsync("Can't show the *'Vueue'* when i'm not playing"); return;
            }

            try
            {
                channel.SendMessageAsync($"Now showing All {player.Vueue.Count} tracks in *'Vueue'*");
                int i = 0;
                foreach (var item in player.Vueue)
                {
                    i++;
                    channel.SendMessageAsync($"Track {i} = {item.Title}");
                }
                return;
            }
            catch (Exception ex)
            {
                channel.SendMessageAsync(ex.Message);
                return;
            }
        }
        public static async Task<string> SeekAsync(TimeSpan timeSpan, IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return "I'm not connected to a voice channel.";
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return "Woaaah there, I can't seek when nothing is playing.";
            }

            try
            {
                await player.SeekAsync(timeSpan);
                return $"I've seeked `{player.Track.Title}` to {timeSpan}.";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        public static async Task<string> VolumeAsync(ushort volume, IGuild guild)
        {
            if (!_lavaNode.TryGetPlayer(guild, out var player))
            {
                return "I'm not connected to a voice channel.";
            }

            try
            {
                await player.SetVolumeAsync(volume);
                return $"I've changed the player volume to {volume}.";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }


        public static async Task TrackEnded(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> args)
        {
            if (loop)
            {
                args.Player.SeekAsync(TimeSpan.Zero); return;
            }

            Console.WriteLine($"Finished playing: [{args.Track.Title}]");
            if (args.Reason == TrackEndReason.LoadFailed) return;
           
            if (!args.Player.Vueue.TryDequeue(out var queueable )) return;

            if (fixVanDeEeuw) { fixVanDeEeuw = false; return; }

            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Next item in *'Vueue'* is not a track");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"Now playing: *{track.Title}* by *{track.Author}*");
        }
    }
}




/*
 * if (user.VoiceChannel is null) return "You must be in a voicechannel!";

            if (!_lavaNode.HasPlayer(guild)) return "I'm not connected to a voicechannel!";


            try
            {
                if(!_lavaNode.TryGetPlayer(guild, out var player))
                {
                    var voiceState = user as IVoiceState;
                    try
                    {
                        player = await _lavaNode.JoinAsync(voiceState.VoiceChannel);
                    }
                    catch (Exception ex)
                    {
                        return $"ERROR:\t{ex.Message}";
                    }
                } 

                LavaTrack track;
                
                var search = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(query, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, query);

                if (search.Status == SearchStatus.LoadFailed) return $"Failed to load {query}";

                track = search.Tracks.FirstOrDefault();

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Vueue.Enqueue(track);
                    Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\tTrack was added to *'Vueue'*");
                    return $"{track.Title} has been added to *'Vueue'*";
                }

                await player.PlayAsync(track);
                Console.WriteLine($"Now playing: {track.Title}");
                return $"Now playing: {track.Title}";
            }

            catch (Exception ex)
            {
                return $"ERROR:\t{ex.Message}";
            }
*/