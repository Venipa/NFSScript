using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFSScript.Core;
using static NFSScript.Core.GameMemory;
using Addrs = NFSScript.Core.UGAddresses;
using NFSScript.Math;


namespace NFSScript.Underground
{
    /// <summary>
    /// A class that represents the game's <see cref="Player"/>.
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// <see cref="Player"/>'s cash (Read only).
        /// </summary>
        public static int Cash
        {
            get
            {
                return memory.ReadInt32((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_CASH);
            }
        }

        /// <summary>
        /// Returns the total amount of money that the <see cref="Player"/> has earned (Read only).
        /// </summary>
        public static int TotalMoneyEarned
        {
            get {
                return memory.ReadInt32((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_TOTAL_MONEY_EARNED);
            }
        }

        /// <summary>
        /// Gets <see cref="Player"/>'s style points (Read only).
        /// </summary>
        public static int StylePoints
        {
            get { return memory.ReadInt32((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_STYLE_POINTS);
            }
        }

        /// <summary>
        /// Award the <see cref="Player"/> with cash.
        /// </summary>
        /// <param name="value"></param>
        public static void AwardCash(int value)
        {
            memory.WriteInt32((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_CASH, Cash + value);
        }

        /// <summary>
        /// Award the <see cref="Player"/> with style points.
        /// </summary>
        /// <param name="value"></param>
        public static void AwardStylePoints(int value)
        {
            memory.WriteInt32((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_STYLE_POINTS, StylePoints + value);
        }

        /// <summary>
        /// A struct that represents the <see cref="Player"/>'s current car
        /// </summary>
        public static class Car
        {
            /// <summary>
            /// Returns the <see cref="Player"/> car's current speed in MPH
            /// </summary>
            /// <returns></returns>
            public static float GetSpeed()
            {
                return memory.ReadFloat((IntPtr)Addrs.PlayerAddrs.STATIC_PLAYER_SPEED_MPH);
            }
        }
    }
}
