# Commands

Commands are organized into categories for better readability

## Connection commands

- ```join (j) - Join voice channel<```
- ```leave (l) - Leave voice channel<```
- ```reload - Reload failed APIs<```
- ```logout (exit, quit, bye, bb, b) - Logout and exit<```

## Queuing commands

- ```play (p) - Add tracks<```
    Arguments:
    - ```query (String) - URL```
- ```playshuffled (psh) - Add shuffled tracks<```
    Arguments:
    - ```query (String) - URL```
- ```tms (t) - Place query result to the head<```
    Arguments:
    - ```query (String) - URL```

## Playback commands

- ```pause (ps) - Pause<```
- ```resume (rs) - Resume<```
- ```stop (st) - Stop<```
- ```skip (s) - Skip<```
    Arguments:
    - ```number (Int32) - Number of tracks to skip (optional)```
- ```count (cnt, cn) - Get queue length<```
- ```clear (clr, cl, c) - Clear the queue<```
- ```shuffle (sh) - Shuffle queue<```
- ```seek (sk) - Seek current track<```
    Arguments:
    - ```timespan (String) - Timespan in format HH:MM:SS or MM:SS```
- ```return (rt) - Return track to queue<```
- ```currenttrack (track, tr) - Get current track<```
- ```nexttrack (next, ntr, nex) - Get next track<```
- ```ignore (it, i) - Ignore current track<```
- ```ignoreartist (ia) - Ignore current track artist<```
    Arguments:
    - ```artist_index (Int32) - Artist zero-based index (optional)```

## Debug commands

- ```test - Get "Hello World" response message<```
- ```name - Get origin bot name<```
- ```help (h) - Get help<```
    Arguments:
    - ```command (String) - Command name (optional)```
