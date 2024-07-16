# Commands

Commands are organized into categories for better readability

## Connection commands

- ```join (j) - Join voice channel```  
- ```leave (l) - Leave voice channel```  
- ```apistatus (status) - Get APIs status```  
- ```apireload (reload) - Reload failed APIs```  
- ```playerstatus (plst) - Get player status```  
- ```logout (exit, quit, bye, bb) - Logout and exit```  
- ```shutdown - Force shutdown```  

## Queuing commands

- ```play (p) - Add tracks to the queue```  
    Arguments:
    - ```query (String) - URL```  
    - ```args (String) - Additional queuing paramtetrs (optional)```
        - ```\SH - shuffle```
        - ```\FF - enqueue to the head```
        - ```\T - play immediatly```
        - ```\R - play in radio mode```
        - ```\B - bypass SQL check```  
- ```playshuffled (psh) - Add shuffled tracks to the queue```  
    Arguments:
    - ```query (String) - URL```  
- ```playhead (pf, ff, f) - Add tracks to the head```  
    Arguments:
    - ```query (String) - URL```  
- ```playimmediatly (pi, t, r) - Play the track immediatly```  
    Arguments:
    - ```query (String) - URL```  
- ```playradio (radio, pr) - Play in radio mode```  
    Arguments:
    - ```query (String) - URL```  
- ```playbypass (pb, b) - Play the track without check```  
    Arguments:
    - ```query (String) - URL```  

## Playback commands

- ```pause (ps) - Pause playback```  
- ```resume (rs) - Resume playback```  
- ```stop (st) - Stop playback```  
- ```skip (s) - Skip current track```  
    Arguments:
    - ```number (Int32) - Number of tracks to skip (optional)```  
- ```count (cnt, cn) - Get the number of tracks in the queue```  
- ```clear (clr, cl, c) - Clear track queue```  
- ```shuffle (sh) - Shuffle queue```  
- ```rewind (seek, rw, sk) - Rewind audio stream```  
    Arguments:
    - ```timespan (String) - Timespan in format HH:MM:SS or MM:SS```  
- ```return (rt) - Return the track to the queue```  
- ```currenttrack (track, tr) - Get information about the current track```  
- ```nexttrack (next, ntr) - Get information about the next track```  

## Database commands

- ```ignoretrack (it) - Ignore current track```  
- ```ignoreartist (ia) - Ignore current track artist```  
    Arguments:
    - ```artist_index (Int32) - Artist zero-based index (optional)```  
- ```save - Save tracks```  
- ```savedcount - Get saved tracks count```  
- ```restore - Restore saved tracks```  

## Debug commands

- ```test - Get "Hello World" response message```  
- ```name - Get the display name of the bot```  
- ```echo - Echo the message```  
    Arguments:
    - ```text (String) - Text```  
- ```help (h) - Get help```  
    Arguments:
    - ```command (String) - Command name (optional)```  
