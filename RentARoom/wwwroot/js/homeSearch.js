// Event listener for the search box input

document.addEventListener('DOMContentLoaded', function () {
    console.log("Script loaded and DOM is ready");

    // Get the search input element and add the event listener for 'input' event
    const searchInput = document.getElementById('searchPhrase');
    const suggestionsList = document.getElementById('suggestionsList');
    let currentIndex = -1; // Track current highlighted suggestion
    let suggestionsData = [];

    if (!searchInput || !suggestionsList) {
        console.error("Search input or suggestions list element not found!");
        return;
    }

    if (searchInput) {
        searchInput.addEventListener('input', function (e) {
            const searchTerm = e.target.value.trim();

            if (searchTerm.length < 2) {
                if (suggestionsList) suggestionsList.innerHTML = '';
                return;
            }
            fetch(`/User/Home/SearchSuggestions?searchType=All&userInput=${encodeURIComponent(searchTerm)}`)
                .then(response => {
                    if (!response.ok) throw new Error('Network error');
                    return response.json();
                })
                .then(data => {
                    if (!suggestionsList) return;
                    suggestionsData = data;
                    suggestionsList.innerHTML = '';

                    if (data.length === 0) {
                        suggestionsList.innerHTML = '<li class="list-group-item">No suggestions found</li>';
                        return;
                    }

                    data.forEach((suggestion, index) => {
                        const li = document.createElement('li');
                        li.classList.add('list-group-item');
                        li.textContent = suggestion;

                        // Set data-attribute
                        li.setAttribute('data-index', index);

                        // Add hover effect and click handler
                        li.addEventListener('mouseover', () => highlightSuggestion(index));
                        li.addEventListener('click', () => selectSuggestion(suggestion));

                        suggestionsList.appendChild(li);
                    });

                })
                .catch(error => {
                    console.error('Error fetching suggestions:', error);
                    if (suggestionsList) suggestionsList.innerHTML = '<li class="list-group-item">Error loading suggestions</li>';
                });
        });


        searchInput.addEventListener('keydown', function (e) {
            const items = suggestionsList.querySelectorAll('li');
            if (items.length === 0) return;

            if (e.key === 'ArrowDown') {
                currentIndex = (currentIndex + 1) % items.length;
                highlightSuggestion(currentIndex);
            } else if (e.key === 'ArrowUp') {
                currentIndex = (currentIndex - 1 + items.length) % items.length;
                highlightSuggestion(currentIndex);
            } else if (e.key === 'Enter') {
                if (currentIndex >= 0 && currentIndex < items.length) {
                    selectSuggestion(items[currentIndex].textContent);
                }
            }
        });

        function highlightSuggestion(index) {
            const items = suggestionsList.querySelectorAll('li');
            items.forEach((item, idx) => {
                if (idx === index) {
                    item.classList.add('bg-primary', 'text-white');
                } else {
                    item.classList.remove('bg-primary', 'text-white');
                }
            });
        }

        function selectSuggestion(suggestion) {
            searchInput.value = suggestion;
            searchInput.style.color = 'black';
            suggestionsList.innerHTML = '';
        }

    } else {
        console.error("Search input element not found!");
    }
});
// #endregion