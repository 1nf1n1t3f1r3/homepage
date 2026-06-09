import pandas as pd
import matplotlib.pyplot as plt
from sklearn.model_selection import KFold

# Load the dataset
file_path = 'Opti-Russell_5000_2018_2023_Part1.csv'
# file_path = 'gaps_cleaned_optimization_results.csv'
data = pd.read_csv(file_path)

# Updated Parameters to evaluate
param_columns = [
    'Base ATR Multiplier', 'RR Ratio', 'ATR Trail Decay',
    'Use Trailing Stop', 'Use Close as Trailing Stop',
    'Use Recent Structure for Trailing Stop', 'Close upon Red Entry'
]

# Function to evaluate parameter combinations
def evaluate_params(data, param_columns):
    grouped_data = data.groupby(param_columns)['Average R-Multiple'].agg(['mean', 'std']).reset_index()
    grouped_data.columns = param_columns + ['Mean Average R-Multiple', 'Std Dev Average R-Multiple']
    return grouped_data

# K-Fold Cross-Validation
kf = KFold(n_splits=5)
results = []

for train_index, test_index in kf.split(data):
    train_data, test_data = data.iloc[train_index], data.iloc[test_index]
    result = evaluate_params(train_data, param_columns)
    results.append(result)

# Aggregate results from all folds
cv_results = pd.concat(results).groupby(param_columns).agg({
    'Mean Average R-Multiple': ['mean', 'std'],
    'Std Dev Average R-Multiple': ['mean', 'std']
}).reset_index()

# Flatten multi-level columns correctly
cv_results.columns = [' '.join(col).strip() if isinstance(col, tuple) else col for col in cv_results.columns]

# Separate plots for different trailing stop strategies
trailing_stop_strategies = [
    {'Use Trailing Stop': True, 'Use Close as Trailing Stop': True, 'Use Recent Structure for Trailing Stop': False,
     'Close upon Red Entry': True},
    {'Use Trailing Stop': True, 'Use Close as Trailing Stop': False, 'Use Recent Structure for Trailing Stop': True,
     'Close upon Red Entry': True},
    {'Use Trailing Stop': True, 'Use Close as Trailing Stop': False, 'Use Recent Structure for Trailing Stop': False,
     'Close upon Red Entry': True},
    {'Use Trailing Stop': False, 'Use Close as Trailing Stop': None, 'Use Recent Structure for Trailing Stop': None,
     'Close upon Red Entry': True},

    {'Use Trailing Stop': True, 'Use Close as Trailing Stop': True, 'Use Recent Structure for Trailing Stop': False,
     'Close upon Red Entry': False},
    {'Use Trailing Stop': True, 'Use Close as Trailing Stop': False, 'Use Recent Structure for Trailing Stop': True,
     'Close upon Red Entry': False},
    {'Use Trailing Stop': True, 'Use Close as Trailing Stop': False, 'Use Recent Structure for Trailing Stop': False,
     'Close upon Red Entry': False},
    {'Use Trailing Stop': False, 'Use Close as Trailing Stop': None, 'Use Recent Structure for Trailing Stop': None,
     'Close upon Red Entry': False}
]

for strategy in trailing_stop_strategies:
    fig = plt.figure(figsize=(10, 8))
    ax = fig.add_subplot(111, projection='3d')

    subset = cv_results
    for key, value in strategy.items():
        subset = subset[subset[key] == value]

    # Plotting
    sc = ax.scatter(
        subset['Base ATR Multiplier'],
        subset['RR Ratio'],
        subset['Mean Average R-Multiple mean'],
        c=subset['ATR Trail Decay'],
        cmap='viridis'
    )

    ax.set_xlabel('Base ATR Multiplier')
    ax.set_ylabel('RR Ratio')
    ax.set_zlabel('Mean Average R-Multiple')
    ax.set_title(f"3D Plot: Strategy - {strategy}")

    cbar = plt.colorbar(sc, ax=ax, pad=0.1)
    cbar.set_label('ATR Trail Decay')

    plt.show()
