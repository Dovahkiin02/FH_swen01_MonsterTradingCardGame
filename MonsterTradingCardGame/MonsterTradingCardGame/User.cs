using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame {
    public enum Role {
        ADMIN,
        PLAYER
    }
    internal record User (string username, string password, int coins, Role role, int wins = 0, int defeats = 0, int ties = 0);
}
