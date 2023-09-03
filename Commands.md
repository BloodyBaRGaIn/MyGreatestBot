# Commands

Commands are organized into categories for better readability

## Connection commands

- <pre>join (j) - Join voice channel</pre>
- <pre>leave (l) - Leave voice channel</pre>
- <pre>reload - Reload failed APIs</pre>
- <pre>logout (exit, quit, bye, bb, b) - Logout and exit</pre>

## Enqueue commands

- <pre>play (p) - Add tracks</pre>
    Arguments:
    - <pre>query (String) - URL</pre>
- <pre>playshuffled (psh) - Add shuffled tracks</pre>
    Arguments:
    - <pre>query (String) - URL</pre>
- <pre>tms (t) - Place query result to the head</pre>
    Arguments:
    - <pre>query (String) - URL</pre>

## Playback commands

- <pre>pause (ps) - Pause</pre>
- <pre>resume (rs) - Resume</pre>
- <pre>stop (st) - Stop</pre>
- <pre>skip (s) - Skip</pre>
    Arguments:
    - <pre>number (Int32) - Number of tracks to skip (optional)</pre>
- <pre>count (cnt, cn) - Get queue length</pre>
- <pre>clear (clr, cl, c) - Clear the queue</pre>
- <pre>shuffle (sh) - Shuffle queue</pre>
- <pre>seek (sk) - Seek current track</pre>
    Arguments:
    - <pre>timespan (String) - Timespan in format HH:MM:SS or MM:SS</pre>
- <pre>return (rt) - Return track to queue</pre>
- <pre>currenttrack (track, tr) - Get current track</pre>
- <pre>nexttrack (next, ntr, nex) - Get next track</pre>
- <pre>ignore (it, i) - Ignore current track</pre>
- <pre>ignoreartist (ia) - Ignore current track artist</pre>
    Arguments:
    - <pre>artist_index (Int32) - Artist zero-based index (optional)</pre>

## Debug commands

- <pre>test - Get "Hello World" response message</pre>
- <pre>name - Get origin bot name</pre>
- <pre>help (h) - Get help</pre>
    Arguments:
    - <pre>command (String) - Command name (optional)</pre>
