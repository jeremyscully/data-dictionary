// Theme Switcher JavaScript

// Apply theme immediately when script loads
(function() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        applyTheme(savedTheme);
    }
})();

document.addEventListener('DOMContentLoaded', function() {
    // Check if a theme is saved in localStorage
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        // Theme already applied above, just update the UI
        
        // Update the active theme option
        const themeOptions = document.querySelectorAll('.theme-option');
        themeOptions.forEach(option => {
            if (option.dataset.theme === savedTheme) {
                option.classList.add('active');
            } else {
                option.classList.remove('active');
            }
        });
    }
    
    // Add click event listeners to theme options
    const themeOptions = document.querySelectorAll('.theme-option');
    themeOptions.forEach(option => {
        option.addEventListener('click', function() {
            const theme = this.dataset.theme;
            
            // Remove active class from all options
            themeOptions.forEach(opt => opt.classList.remove('active'));
            
            // Add active class to selected option
            this.classList.add('active');
            
            // Apply the selected theme
            applyTheme(theme);
            
            // Save the theme preference to localStorage
            localStorage.setItem('theme', theme);
        });
    });
});

// Function to apply the selected theme
function applyTheme(theme) {
    const body = document.body;
    
    // Remove all theme classes
    body.classList.remove('netflix-theme');
    body.classList.remove('seattle-housing-theme');
    
    // Add the selected theme class if it's not the default
    if (theme !== 'default') {
        body.classList.add(`${theme}-theme`);
    }
    
    // Update theme name in the header if it exists
    const themeNameElement = document.getElementById('current-theme-name');
    if (themeNameElement) {
        // Format the theme name for display (capitalize first letter of each word)
        let displayName = theme.split('-').map(word => 
            word.charAt(0).toUpperCase() + word.slice(1)
        ).join(' ');
        
        themeNameElement.textContent = displayName;
    }
} 