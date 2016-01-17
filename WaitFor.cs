/*
 * WaitFor version 1.1
 *
 * Copyright(c) shtonki 2016
 *
 *
 * Permission is hereby granted, free of charge, to any person obtaining 
 * a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.

 * The Software shall be used for Good, not Evil.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE
 */

using System.Threading;


/// <summary>
/// Provides a class which allows one thread to wait for another thread
/// to provide a value of a given type.
/// </summary>
/// <typeparam name="T">The type of the value to be produced</typeparam>
class WaitFor<T>
{
    private ManualResetEvent e;
    private T val;
    private bool valid;

    /// <summary>
    /// Creates a new WaitFor instance ready to be waited on.
    /// </summary>
    public WaitFor()
    {
        e = new ManualResetEvent(false);
    }

    /// <summary>
    /// Blocks until another thread calls signal on this WaitFor.
    /// </summary>
    /// <returns>The value provided by the next call to signal</returns>
    public T wait()
    {
        valid = true;
        e.Reset();
        e.WaitOne();
        valid = false;
        return val;
    }

    /// <summary>
    /// Signals all threads waiting on this instance with the given value.
    /// </summary>
    /// <param name="t">The value to give the threads waiting on this instance.</param>
    /// <returns>True if atleast one thread received the given value, false otherwise</returns>
    public bool signal(T t)
    {
        bool r = valid;
        val = t;
        e.Set();
        return r;
    }
}