﻿using System;
using ReldawinServerMaster.Properties;

// code by cliss on Github
// https://gist.github.com/cliss/f03f7268a1c9006daf88

public class DebugTimer : IDisposable
{
    private readonly System.Diagnostics.Stopwatch _watch;
    private readonly string _blockName;

    /// <summary>
    /// Creates a timer.
    /// </summary>
    /// <param name="blockName">Name of the block that's being timed</param>
    public DebugTimer( string blockName )
    {
        _blockName = blockName;
        _watch = System.Diagnostics.Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _watch.Stop();
        GC.SuppressFinalize( this );
        if(Config.Debugging)
            Console.WriteLine( _watch.Elapsed.TotalMilliseconds + "ms to call " + _blockName );
    }

    ~DebugTimer()
    {
        throw new InvalidOperationException( "Must Dispose() of all instances of " + this.GetType().FullName );
    }
}