import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { DataBrowser } from './components/ArtistDataBrowser';
import { CompareArtists } from './components/CompareArtists';
import { FetchTracks } from './components/FetchTracks';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/DataBrowser' component={DataBrowser} />
        <Route path='/CompareArtists' component={CompareArtists} />
        <Route path='/fetch-tracks' component={FetchTracks} />
      </Layout>
    );
  }
}
