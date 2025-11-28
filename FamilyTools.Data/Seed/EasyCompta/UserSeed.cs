using System;
using System.Collections.Generic;
using System.Text;

using FamilyTools.Data.Context;
using FamilyTools.Data.Models.EasyCompta;

namespace FamilyTools.Data.Seed.EasyCompta
{
    public class UserSeed(EasyComptaContext EasyComptaContext) : IContextSeed
    {
        public EasyComptaContext Context { get; set; } = EasyComptaContext;

        public async Task Execute()
        {
            if (this.Context.Users.Any()) return;

            var newUser = new List<User>() {
                new(){
                    FirstName = "John",
                    LastName = "Flagiu",
                    UserName = "Jojo"
                },
                new(){
                    FirstName = "Lili",
                    LastName = "Luthe",
                    UserName = "Lili"
                }
            }; 

            this.Context.Users.AddRange(newUser);
            await Context.SaveChangesAsync();

        }
    }
}
