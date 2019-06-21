using System;

// location type  block              major     minor     floor
// -------------  ---------          --------  --------  --------
// STOWAGE        DECK/HOLD          bay nr    row nr    floor nr
// QCTP           QC id              0         lane nr   floor nr
// WSTP           block id           stack nr  lane nr   floor nr
// LSTP           block id           stack nr  lane nr   floor nr
// YARD           block id           stack nr  lane nr   floor nr
// SCPARK         sc id              0         0         0

namespace QSim.ConsoleApp.DataTypes
{
    public class Location
    {
        public LocationType locationType;
        public int block, major, minor, floor;
        
        public Location(LocationType _type, int _block, int _major, int _minor, int _floor)
        {
            locationType = _type;

            block = _block;
            major = _major;
            minor = _minor;
            floor = _floor;
        }

        public override bool Equals(object othObj)
        {
            if (othObj is Location otherLocation)
            {
                return locationType == otherLocation.locationType &&
                       block == otherLocation.block &&
                       major == otherLocation.major &&
                       minor == otherLocation.minor &&
                       floor == otherLocation.floor;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(locationType, block, major, minor, floor);
        }

        public override string ToString()
        {
            return $"{locationType}({block}, {major}, {minor}, {floor})";
        }

        public Location Copy()
        {
            return new Location(locationType, block, major, minor, floor);
        }
    }
}
