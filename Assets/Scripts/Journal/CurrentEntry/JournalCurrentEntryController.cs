using Journal.JournalCurrentEntry;
using LD48.AudioTool;
using Pagination;
using Signals;
using Utilities.Monads;

namespace Journal
{
    public class JournalCurrentEntryController
    {
        private readonly JournalCurrentEntryView view;
        private readonly IPaginationController paginationController;
        private JournalEntry journalEntry;

        public JournalCurrentEntryController(JournalCurrentEntryView view, IPaginationController paginationController)
        {
            this.view = view;
            this.paginationController = paginationController;
        }

        public void SetUp(IMaybe<JournalEntry> maybeJournalEntry)
        {
            maybeJournalEntry.IfPresent(entry =>
            {
                journalEntry = entry;
                view.EntryNameText.text = journalEntry.EntryTitle;
                view.EntryDescriptionText.text = entry.EntryDescriptions[0];
                paginationController.SetUp(new PaginationConfiguration
                {
                    OnPreviousPage = OnPreviousPage,
                    OnNextPage = OnNextPage,
                    PageCount = journalEntry.EntryDescriptions.Count,
                    StartingPageIndex = 0,
                    ShowCurrentPageText = true
                });
            }).IfNotPresent(() =>
            {
                view.EntryNameText.text = string.Empty;
                view.EntryDescriptionText.text = string.Empty;
                paginationController.SetUp(new PaginationConfiguration());
            });
            view.Refresh();
        }

        private void OnPreviousPage(int pageIndex)
        {
            // TODO: FadeIn/FadeOut Animation
            SignalsHub.DispatchAsync(new PlaySoundSignal{ Name = SoundName.FlipPageSound });
            view.EntryDescriptionText.text = journalEntry.EntryDescriptions[pageIndex];
        }

        private void OnNextPage(int pageIndex)
        {
            // TODO: FadeIn/FadeOut Animation
            SignalsHub.DispatchAsync(new PlaySoundSignal{ Name = SoundName.FlipPageSound });
            view.EntryDescriptionText.text = journalEntry.EntryDescriptions[pageIndex];
        }
    }
}