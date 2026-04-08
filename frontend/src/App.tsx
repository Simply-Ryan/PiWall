import React, { useEffect } from 'react';
import { StatusBar } from 'expo-status-bar';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { Provider } from 'react-redux';
import { store } from '@redux/store';
import { HomeScreen } from '@screens/HomeScreen';

const Stack = createNativeStackNavigator();

/**
 * Root application component
 *
 * Sets up Redux provider, navigation, and status bar configuration
 */
export const App: React.FC = () => {
  useEffect(() => {
    // TODO: Initialize application state and connections
    // - Connect to Telemetry Bridge WebSocket
    // - Verify backend API availability
    // - Load user preferences
  }, []);

  return (
    <Provider store={store}>
      <StatusBar barStyle="light-content" backgroundColor="#000000" />
      <NavigationContainer>
        <Stack.Navigator
          screenOptions={{
            headerShown: false,
            gestureEnabled: false,
          }}
        >
          <Stack.Screen
            name="Home"
            component={HomeScreen}
            options={{ animationEnabled: false }}
          />
          {/* Additional screens will be added during Phase 2 */}
        </Stack.Navigator>
      </NavigationContainer>
    </Provider>
  );
};

export default App;
