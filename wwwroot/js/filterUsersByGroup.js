document.addEventListener("DOMContentLoaded", function () {

    const usersByTeamElement = document.getElementById("usersByTeamData");
    const usersByTeam = JSON.parse(usersByTeamElement.textContent);

    const assignedUserSelect = document.getElementById("assignedUserSelect");
    const assignedTeamSelect = document.getElementById("assignedTeamSelect");

    // When a user is selected, set the corresponding team
    assignedUserSelect.addEventListener("change", function () {
        const selectedUserId = this.value;

        for (const [teamId, users] of Object.entries(usersByTeam)) {
            if (users.some(user => user.Id == selectedUserId)) {
                assignedTeamSelect.value = teamId; 
                break; 
            }
         }
    })

    // When a team is selected, filter the AssignedUser select options
    assignedTeamSelect.addEventListener("change", function() {
        const selectedTeamId = this.value; 

        // If no value is selected, show all users 
        if (!selectedTeamId) {
            assignedUserSelect.innerHTML = '<option value="">-- Select --</option>'; // Reset options

            for (const team in usersByTeam) {
                usersByTeam[team].forEach(user => {
                    const option = document.createElement("option");
                    option.value = user.Id;
                    option.textContent = user.Name;
                    assignedUserSelect.appendChild(option);
                })
            }
            return;
        }

        assignedUserSelect.innerHTML = '<option value="">-- Select --</option>'; // Reset options

        if (usersByTeam[selectedTeamId]) {
            usersByTeam[selectedTeamId].forEach(user => {
                const option = document.createElement("option");
                option.value = user.Id;
                option.textContent = user.Name;
                assignedUserSelect.appendChild(option); 
            })
        }
    })
})