using Newtonsoft.Json.Linq;
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
        string name,
        Role role,
        int coins,
        int wins = 0,
        int defeats = 0,
        int draws = 0,
        int elo = 0) {
        public JObject toResponseObject() {
            return new JObject() {
                ["id"] = id,
                ["name"] = name,
                ["role"] = Enum.GetName(typeof(Role), role),
                ["coins"] = coins,
                ["wins"] = wins,
                ["defeats"] = defeats,
                ["draws"] = draws,
                ["elo"] = elo
            };
        }

        public JObject toScoreBoardResponseObject() {
            return new() {
                ["name"] = name,
                ["wins"] = wins,
                ["defeats"] = defeats,
                ["draws"] = draws,
                ["statistic"] = defeats == 0 ? wins : wins / defeats,
                ["elo"] = elo
            };
        }
    }
}
