/***
 * contacts - {}
 *      [Number - int]: Name - String (50)
 *
 */


/***
 * Blacklist - [Number - int...]
 *
 */

/***
 * Messages - {}
 *      [Number - int] - {}
 *          Time - DateTime
 *          Type - Tynyint
 *          Text - Text
 *          Data - Text
 */

/***
 * Recents - [{}...]
 *      Number - int
 *      Time - DateTime
 *
 */

// Phone contacts data store
export const contactsData = {
    contacts: {},
    blacklist: [],
    messages: {},
    recents: []
};

// Initialize contacts from server data
export const initContacts = (json) => {
    try {
        const data = JSON.parse(json);
        if (data.contacts) contactsData.contacts = data.contacts;
        if (data.blacklist) contactsData.blacklist = data.blacklist;
        if (data.messages) contactsData.messages = data.messages;
        if (data.recents) contactsData.recents = data.recents;
    } catch (e) {
        // Failed to parse contacts data
    }
};

// Get contact name by phone number
export const getContactName = (number) => {
    return contactsData.contacts[number] || null;
};

// Check if number is in blacklist
export const isBlacklisted = (number) => {
    return contactsData.blacklist.includes(number);
};

// Get messages for a specific number
export const getMessages = (number) => {
    return contactsData.messages[number] || [];
};

// Get recent calls list
export const getRecents = () => {
    return contactsData.recents;
};