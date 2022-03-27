import ButtonLoading from "./ButtonLoading";
import Errors from "./Errors";
import Modals from "./Modals";
import Popup from "./Popup";
import Ajax from "./Ajax";

class Lists
{
    constructor()
    {
        this.uiRenameModal       = $('.list_rename_modal');
        this.uiRenameModalButton = $('.link_rename_list');
        this.uiRenameForm        = $('.rename_list_form');
    }

    watch()
    {
        console.log("Lists.Watch()");
        // add modals
        Modals.add(this.uiRenameModal, this.uiRenameModalButton);

        // on submitting a new
        this.uiRenameForm.on('submit', event => {
            event.preventDefault();
            const listId = this.uiRenameForm.attr('data-id');
            this.rename(
                this.uiRenameForm.find('#list_name').val().trim(),
                listId
            );
        });
    }
    /**
     * Create a new list
     */
    rename(name, listId)
    {
        const $button = this.uiRenameForm.find('button[type="submit"]');
        ButtonLoading.start($button);

        if (name.length < 3) {
            Popup.error('name too short', 'Your list name is a bit too short, please enter in a name to create a list!');
            return;
        }
        
        const data = {
            name: name
        };

        const success = response => {
            Modals.close(this.uiRenameModal);
            Popup.success(
                'Renamed list',
                'You have renamed your list!'
            );
            window.location.reload();
        };

        const complete = () => {
            ButtonLoading.finish($button);
        };

        Ajax.post(mog.urls.lists.rename.replace('-id-', listId), data, success, complete, Errors.lists.couldNotRenameList);
    }

}

export default new Lists;
