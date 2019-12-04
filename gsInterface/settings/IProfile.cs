﻿namespace gs.interfaces
{
    public interface IProfile
    {
        string ManufacturerName { get; }
        string ModelIdentifier { get; }
        string ProfileName { get; }

        double MachineBedSizeXMM { get; }
        double MachineBedSizeYMM { get; }
        double MachineBedSizeZM01M { get; }

        IProfile Clone();
    }
}
