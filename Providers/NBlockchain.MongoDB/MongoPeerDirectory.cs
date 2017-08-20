﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using NBlockChain.Interfaces;
using NBlockChain.Models;
using Newtonsoft.Json.Linq;

namespace NBlockchain.MongoDB
{
    public class MongoPeerDirectory : IPeerDiscoveryService
    {
        private readonly IMongoDatabase _database;

        public MongoPeerDirectory(IMongoDatabase database)
        {
            _database = database;
            //CreateIndexes(this);
        }
        
        private IMongoCollection<MongoPeerNode> Peers => _database.GetCollection<MongoPeerNode>("nbc.peers");

        
        public async Task<ICollection<KnownPeer>> DiscoverPeers()
        {
            var query = await Peers.FindAsync(x => true);
            var raw = await query.ToListAsync();
            return raw.Cast<KnownPeer>().ToList();
        }

        public async Task SharePeers(ICollection<KnownPeer> peers)
        {
            foreach (var peer in peers)
            {
                var query = Peers.Find(x => x.ConnectionString == peer.ConnectionString);
                if (query.Any())
                {
                    var existing = query.First();
                    Peers.ReplaceOne(x => x.Id == existing.Id, new MongoPeerNode(peer));
                }
                else
                {
                    Peers.InsertOne(new MongoPeerNode(peer));
                }
            }
        }

        static bool indexesCreated = false;
        static void CreateIndexes(MongoPeerDirectory instance)
        {
            if (!indexesCreated)
            {
                //TODO
                indexesCreated = true;
            }
        }

        public async Task AdvertiseGlobal(string connectionString)
        {         
        }

        public async Task AdvertiseLocal(string connectionString)
        {
        }
    }

    public class MongoPeerNode : KnownPeer
    {
        public ObjectId Id { get; set; }

        public MongoPeerNode()
        {
        }

        public MongoPeerNode(KnownPeer node)
        {
            this.ConnectionString = node.ConnectionString;
            this.LastContact = node.LastContact;
        }
    }
}