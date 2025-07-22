namespace PostalRegistry.PostalInformation
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.BPost;
    using Events.Crab;

    public partial class PostalInformation
    {
        private PostalInformationStatus? _status;
        private readonly List<PostalName> _postalNames = new List<PostalName>();

        public PostalCode PostalCode { get; private set; }
        public IReadOnlyCollection<PostalName> PostalNames => _postalNames.AsReadOnly();
        public NisCode? NisCode { get; set; }
        public Modification LastModification { get; private set; }
        public bool IsRemoved { get; private set; } = false;

        public PostalInformation()
        {
            Register<PostalInformationWasRegistered>(When);
            Register<PostalInformationWasRealized>(When);
            Register<PostalInformationWasRetired>(When);
            Register<PostalInformationPostalNameWasAdded>(When);
            Register<PostalInformationPostalNameWasRemoved>(When);
            Register<MunicipalityWasAttached>(When);
            Register<MunicipalityWasRelinked>(When);
            Register<PostalInformationWasRemoved>(When);

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

        private void When(PostalInformationWasRealized @event)
        {
            _status = PostalInformationStatus.Current;
        }

        private void When(PostalInformationWasRegistered @event)
        {
            PostalCode = new PostalCode(@event.PostalCode);
        }

        private void When(MunicipalityWasAttached @event)
        {
            NisCode = new NisCode(@event.NisCode);
        }

        private void When(MunicipalityWasRelinked @event)
        {
            NisCode = new NisCode(@event.NewNisCode);
        }

        private void When(PostalInformationWasRemoved @event)
        {
            IsRemoved = true;
        }
    }
}
