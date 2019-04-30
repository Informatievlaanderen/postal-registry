namespace PostalRegistry.PostalInformation
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.BPost;
    using Events.Crab;

    public partial class PostalInformation
    {
        public PostalCode PostalCode { get; private set; }
        private PostalInformationStatus? _status;

        private NisCode _nisCode;

        private readonly List<PostalName> _postalNames = new List<PostalName>();

        public Modification LastModification { get; private set; }

        public PostalInformation()
        {
            Register<PostalInformationWasRegistered>(When);
            Register<PostalInformationBecameCurrent>(When);
            Register<PostalInformationWasRetired>(When);
            Register<PostalInformationPostalNameWasAdded>(When);
            Register<PostalInformationPostalNameWasRemoved>(When);
            Register<MunicipalityWasLinkedToPostalInformation>(When);

            Register<PostalInformationWasImportedFromBPost>(@event => WhenCrabEventApplied());
            Register<PostalInformationWasImportedFromCrab>(@event => WhenCrabEventApplied());
        }

        private void WhenCrabEventApplied()
        {
            switch (LastModification)
            {
                case Modification.Unknown:
                    LastModification = Modification.Insert;
                    break;

                case Modification.Insert:
                    LastModification = Modification.Update;
                    break;
            }
        }

        private void When(PostalInformationPostalNameWasRemoved @event)
        {
            _postalNames.Remove(new PostalName(@event.Name, @event.Language));
        }

        private void When(PostalInformationPostalNameWasAdded @event)
        {
            _postalNames.Add(new PostalName(@event.Name, @event.Language));
        }

        private void When(PostalInformationWasRetired @event)
        {
            _status = PostalInformationStatus.Retired;
        }

        private void When(PostalInformationBecameCurrent @event)
        {
            _status = PostalInformationStatus.Current;
        }

        private void When(PostalInformationWasRegistered @event)
        {
            PostalCode = new PostalCode(@event.PostalCode);
        }

        private void When(MunicipalityWasLinkedToPostalInformation @event)
        {
            _nisCode = new NisCode(@event.NisCode);
        }
    }
}
