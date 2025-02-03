document.addEventListener("DOMContentLoaded", function () {

    const usersByTeamElement = document.getElementById("usersByTeamData");
    const usersByTeam = JSON.parse(usersByTeamElement.textContent);

    const assignedUserSelect = document.getElementById("assignedUserSelect");
    const assignedTeamSelect = document.getElementById("assignedTeamSelect");

    // When a user is selected, set the corresponding team
    assignedUserSelect.addEventListener("change", function () {
        const selectedUserId = this.value;

        for (const [teamId, users] of Object.entries(usersByTeam)) {
            if (users.some(user => user.id == selectedUserId)) {
                assignedTeamSelect.value = teamId; 
                break; 
            }
         }
    })

    // When a team is selected, filter the AssignedUser select options
    assignedTeamSelect.addEventListener("change", function() {
        const selectedTeamId = this.value; 

        // if no value is selected, show all users
        console.log(selectedTeamId == false); 
        if (!selectedTeamId) {
            assignedUserSelect.innerHTML = '<option value="">-- Select --</option>'; // Reset options

            for (const team in usersByTeam) {
                console.log(team);
                usersByTeam[team].forEach(user => {
                    const option = document.createElement("option");
                    option.value = user.id;
                    option.textContent = user.name;
                    assignedUserSelect.appendChild(option);
                })
            }
            return;
        }

        assignedUserSelect.innerHTML = '<option value="">-- Select --</option>'; // Reset options

        if (usersByTeam[selectedTeamId]) {
            usersByTeam[selectedTeamId].forEach(user => {
                const option = document.createElement("option");
                option.value = user.id;
                option.textContent = user.name;
                assignedUserSelect.appendChild(option); 
            })
        }
    })
})