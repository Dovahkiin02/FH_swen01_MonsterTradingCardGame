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
    internal record User (
        Guid id,
        string username,
        Role role,
        int coins,
        int wins = 0,
        int defeats = 0,
        int ties = 0);
}
