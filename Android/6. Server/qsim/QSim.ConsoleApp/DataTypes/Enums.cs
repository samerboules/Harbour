using System;
using System.Collections.Generic;
using System.Text;

namespace QSim.ConsoleApp.DataTypes
{
    public enum LocationType {
        STOWAGE,
        QCTP,
        WSTP,
        LSTP,
        YARD,
        HIGHWAY,
        SCPARK
    }

    public enum QCTPSubPosition
    {
        LEFT,
        CENTER,
        RIGHT
    }
    public enum ContainerHeight
    {
        UNKNOWN,
        HEIGHT_8_0,
        HEIGHT_9_0,
        HEIGHT_8_6,
        HEIGHT_9_6
    }

    public enum ContainerLength
    {
        UNKNOWN,
        LENGTH_20,
        LENGTH_40,
        LENGTH_45
    }

    public enum ContainerOrientationAtWSTP
    {
        WATERSIDE,
        LANDSIDE
    }

    public enum ContainerOrientationAtQCTP
    {
        LEFT,
        RIGHT
    }
    public enum ObjectType
    {
        ASC,
        QC,
        AUTOSTRAD,
        CONTAINER_20,
        CONTAINER_40,
        CLAIM
    }

    public enum MessageType
    {
        UPDATE,
        DELETE,
        PICKUP,
        PUTDOWN,
        SPREADER,
        SPREADER_SIZE,
        CLAIM,
        STATUS
    }

    public enum SpreaderSize
    {
        SPREADER_40,
        SPREADER_20,
        SPREADER_TWIN_20
    }
}
