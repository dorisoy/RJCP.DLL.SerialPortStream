﻿// $URL$
// $Id$

// Copyright © Jason Curl 2012-2014.
// See http://serialportstream.codeplex.com for license details (MS-PL License)

namespace RJCP.IO
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Provides a local implementation of an IAsyncResult
    /// </summary>
    internal class LocalAsync : IAsyncResult, IDisposable
    {
        private object m_State;
        private ManualResetEvent m_Handle;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAsync"/> class.
        /// </summary>
        /// <param name="state">The state object given by the user in a BeginWrite/EndWrite method.</param>
        /// <remarks>
        /// When your asynchronous operation is finished, you should set the <see cref="IsCompleted"/>
        /// property, which will automatically trigger the <see cref="AsyncWaitHandle"/> if the user is
        /// waiting on this. When you're finished, be sure to call the <see cref="Dispose()"/> method in
        /// your EndXXX() method.
        /// </remarks>
        public LocalAsync(object state)
        {
            m_State = state;
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information
        /// about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains
        /// information about an asynchronous operation.</returns>
        public object AsyncState
        {
            get { return m_State; }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle" /> that
        /// is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle" /> that is
        /// used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (m_Handle == null) {
                    bool done = IsCompleted;
                    ManualResetEvent mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref m_Handle,
                        mre, null) != null) {
                        // Another thread created this object's event; dispose 
                        // the event we just created
                        mre.Close();
                    } else {
                        if (!done && IsCompleted) {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
                            m_Handle.Set();
                        }
                    }
                }
                return m_Handle;
            }
        }

        private bool m_IsSynch;

        /// <summary>
        /// Gets a value that indicates whether the asynchronous
        /// operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously;
        /// otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get { return m_IsSynch; }
            internal set { m_IsSynch = value; }
        }

        private volatile bool m_IsCompleted;

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted
        {
            get { return m_IsCompleted; }
            set
            {
                if (value == true) {
                    m_IsCompleted = true;
                    if (m_Handle != null) m_Handle.Set();
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                if (m_Handle != null) {
                    m_Handle.Dispose();
                    m_Handle = null;
                }
            }
        }
    }

    internal class LocalAsync<T> : LocalAsync
    {
        private T m_Result;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAsync"/> class.
        /// </summary>
        /// <param name="state">The state object given by the user in a BeginWrite/EndWrite method.</param>
        /// <remarks>
        /// When your asynchronous operation is finished, you should set the <see cref="LocalAsync.IsCompleted"/>
        /// property, which will automatically trigger the <see cref="LocalAsync.AsyncWaitHandle"/> if the user is
        /// waiting on this. When you're finished, be sure to call the <see cref="LocalAsync.Dispose()"/> method in
        /// your EndXXX() method.
        /// </remarks>
        public LocalAsync(object state) : base(state) { }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        internal T Result
        {
            get { return m_Result; }
            set { m_Result = value; }
        }
    }
}
