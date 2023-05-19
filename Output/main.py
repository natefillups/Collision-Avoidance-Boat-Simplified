import pandas as pd
import matplotlib.pyplot as plt

# Load the Excel file into a Pandas dataframe
df = pd.read_excel('output.xlsx')

# Extract the values from the first two columns
x = df.iloc[:, 0]
y = df.iloc[:, 1]

# Create a scatter plot of the data
plt.scatter(x, y)

# Add labels and title to the plot
plt.xlabel('Column 1')
plt.ylabel('Column 2')
plt.title('Plot of First Two Columns')

# Show the plot
plt.show()
