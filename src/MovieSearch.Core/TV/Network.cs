﻿using System.Collections.Generic;

namespace MovieSearch.Core.TV;

public class Network : IEqualityComparer<Network>
{
    public Network(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }
    public string Name { get; init; }

    public bool Equals(Network x, Network y)
    {
        return x != null && y != null && x.Id == y.Id && x.Name == y.Name;
    }

    public int GetHashCode(Network obj)
    {
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            hash = hash * 23 + obj.Id.GetHashCode();
            hash = hash * 23 + obj.Name.GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not Network network)
            return false;

        return Equals(this, network);
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }
}
