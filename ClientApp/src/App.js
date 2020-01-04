import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { RastaPage } from './components/rastapage';
import { FetchData } from './components/FetchData';
import { FetchTracks } from './components/FetchTracks';
import { Counter } from './components/Counter';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
        <Route path='/RastaPage' component={RastaPage} />
        <Route path='/fetch-data' component={FetchData} />
        <Route path='/fetch-tracks' component={FetchTracks} />
      </Layout>
    );
  }
}
