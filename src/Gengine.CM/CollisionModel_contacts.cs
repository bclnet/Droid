using System.NumericsX;
using CmHandle = System.Int32;

namespace Gengine.CM
{
    partial class CollisionModelManagerLocal
    {
        int Contacts(ContactInfo contacts, int maxContacts, in Vector3 start, in Vector6 dir, in float depth, in TraceModel trm, in Matrix3x3 trmAxis, int contentMask, CmHandle model, in Vector3 origin, in Matrix3x3 modelAxis)
        {
            Trace results;
            Vector3 end;

            // same as Translation but instead of storing the first collision we store all collisions as contacts
            this.getContacts = true;
            this.contacts = contacts;
            this.maxContacts = maxContacts;
            this.numContacts = 0;
            end = start + dir.SubVec3(0) * depth;
            this.Translation(results, start, end, trm, trmAxis, contentMask, model, origin, modelAxis);
            if (dir.SubVec3(1).LengthSqr != 0.0f) { } // FIXME: rotational contacts
            this.getContacts = false;
            this.maxContacts = 0;

            return this.numContacts;
        }
    }
}